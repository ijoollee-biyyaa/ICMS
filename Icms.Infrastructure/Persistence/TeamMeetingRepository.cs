using Microsoft.EntityFrameworkCore;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Persistence;

public class TeamMeetingRepository(IcmsDbContext dbContext) : ITeamMeetingRepository
{
    public Task<Team?> GetTeamAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .SingleOrDefaultAsync(t => t.ChurchId == churchId && t.Id == teamId, ct);

    public Task<bool> HasSubTeamsAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .AnyAsync(t => t.ParentTeamId == teamId && t.ChurchId == churchId, ct);

    public Task<List<long>> GetActiveMemberIdsAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.TeamMembers.AsNoTracking()
            .Where(tm => tm.TeamId == teamId && tm.Team.ChurchId == churchId && !tm.IsDeleted)
            .Select(tm => tm.MemberId)
            .ToListAsync(ct);

    public Task<bool> IsLeaderAsync(long teamId, long memberId, CancellationToken ct) =>
        dbContext.TeamMembers.AsNoTracking()
            .AnyAsync(tm => tm.TeamId == teamId
                && tm.MemberId == memberId
                && tm.Role == TeamMemberRole.Leader
                && !tm.IsDeleted, ct);

    public Task<TeamMeeting?> GetByIdAsync(long teamId, long meetingId, CancellationToken ct) =>
        dbContext.TeamMeetings
            .Include(m => m.CreatedBy)
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.TeamId == teamId, ct);

    public Task<TeamMeeting?> GetByIdWithAttendanceAsync(long teamId, long meetingId, CancellationToken ct) =>
        dbContext.TeamMeetings
            .Include(m => m.CreatedBy)
            .Include(m => m.Attendances).ThenInclude(a => a.Member)
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.TeamId == teamId, ct);

    public Task<List<TeamMeeting>> ListMeetingsAsync(long churchId, long teamId, int page, int pageSize, CancellationToken ct) =>
        dbContext.TeamMeetings.AsNoTracking()
            .Include(m => m.CreatedBy)
            .Where(m => m.TeamId == teamId && m.Team.ChurchId == churchId)
            .OrderByDescending(m => m.MeetingDate)
            .ThenByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountMeetingsAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.TeamMeetings.AsNoTracking()
            .CountAsync(m => m.TeamId == teamId && m.Team.ChurchId == churchId, ct);

    public Task<List<TeamAttendance>> GetCountsAsync(long meetingId, CancellationToken ct) =>
        dbContext.TeamAttendances.AsNoTracking()
            .Where(a => a.MeetingId == meetingId)
            .ToListAsync(ct);

    public async Task<TeamMeeting> CreateAsync(TeamMeeting meeting, CancellationToken ct)
    {
        dbContext.TeamMeetings.Add(meeting);
        await dbContext.SaveChangesAsync(ct);
        return meeting;
    }

    public async Task DeleteAsync(TeamMeeting meeting, CancellationToken ct)
    {
        var attendances = await dbContext.TeamAttendances
            .Where(a => a.MeetingId == meeting.Id)
            .ToListAsync(ct);

        if (attendances.Count > 0)
            dbContext.TeamAttendances.RemoveRange(attendances);

        dbContext.TeamMeetings.Remove(meeting);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<int> UpsertAttendanceAsync(
        long meetingId, long teamId, DateOnly date,
        IReadOnlyList<(long MemberId, TeamAttendanceStatus Status, string? Reason)> entries,
        CancellationToken ct)
    {
        var existing = await dbContext.TeamAttendances
            .Where(a => a.MeetingId == meetingId)
            .ToListAsync(ct);

        var byMember = existing.ToDictionary(a => a.MemberId);

        foreach (var (memberId, status, reason) in entries)
        {
            if (byMember.TryGetValue(memberId, out var row))
            {
                row.Status = status;
                row.Reason = reason;
            }
            else
            {
                dbContext.TeamAttendances.Add(new TeamAttendance
                {
                    TeamId = teamId,
                    MeetingId = meetingId,
                    MemberId = memberId,
                    AttendanceDate = date,
                    Status = status,
                    Reason = reason
                });
            }
        }

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (IsUniqueViolation(ex))
        {
            dbContext.ChangeTracker.Clear();

            var fresh = await dbContext.TeamAttendances
                .Where(a => a.MeetingId == meetingId)
                .ToListAsync(ct);

            var freshByMember = fresh.ToDictionary(a => a.MemberId);

            foreach (var (memberId, status, reason) in entries)
            {
                if (freshByMember.TryGetValue(memberId, out var row))
                {
                    row.Status = status;
                    row.Reason = reason;
                }
                else
                {
                    dbContext.TeamAttendances.Add(new TeamAttendance
                    {
                        TeamId = teamId,
                        MeetingId = meetingId,
                        MemberId = memberId,
                        AttendanceDate = date,
                        Status = status,
                        Reason = reason
                    });
                }
            }

            await dbContext.SaveChangesAsync(ct);
        }

        return entries.Count;
    }

    private static bool IsUniqueViolation(Exception ex)
    {
        for (var e = ex.InnerException; e is not null; e = e.InnerException)
        {
            if (e is Npgsql.PostgresException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation })
                return true;
        }

        return false;
    }
}