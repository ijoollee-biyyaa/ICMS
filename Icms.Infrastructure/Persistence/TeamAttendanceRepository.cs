using Microsoft.EntityFrameworkCore;
using Npgsql;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Persistence;

public class TeamAttendanceRepository(IcmsDbContext dbContext) : ITeamAttendanceRepository
{
    public Task<Team?> GetTeamAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .SingleOrDefaultAsync(t => t.ChurchId == churchId && t.Id == teamId, ct);

    public Task<bool> HasSubTeamsAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .AnyAsync(t => t.ParentTeamId == teamId && t.ChurchId == churchId, ct);

    public Task<List<long>> GetActiveMemberIdsAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.TeamMembers.AsNoTracking()
            .Where(tm => tm.TeamId == teamId && tm.Team.ChurchId == churchId)
            .Select(tm => tm.MemberId)
            .ToListAsync(ct);

    public async Task<int> UpsertAsync(long teamId, DateOnly date,
        IReadOnlyList<(long MemberId, TeamAttendanceStatus Status, string? Reason)> entries, CancellationToken ct)
    {
        var existing = await dbContext.TeamAttendances
            .Where(a => a.TeamId == teamId && a.AttendanceDate == date)
            .ToListAsync(ct);

        ApplyChanges(existing, teamId, date, entries);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            dbContext.ChangeTracker.Clear();

            var fresh = await dbContext.TeamAttendances
                .Where(a => a.TeamId == teamId && a.AttendanceDate == date)
                .ToListAsync(ct);

            ApplyChanges(fresh, teamId, date, entries);

            await dbContext.SaveChangesAsync(ct);
        }

        return entries.Count;
    }

    public Task<List<TeamAttendance>> GetByTeamAsync(long churchId, long teamId,
        DateOnly? from, DateOnly? to, string? search, int page, int pageSize, CancellationToken ct) =>
        dbContext.TeamAttendances.AsNoTracking()
            .Include(a => a.Member)
            .Where(a => a.TeamId == teamId && a.Team.ChurchId == churchId
                && (from == null || a.AttendanceDate >= from)
                && (to == null || a.AttendanceDate <= to)
                && (search == null || EF.Functions.ILike(a.Member.FirstName, $"%{search}%")))
            .OrderByDescending(a => a.AttendanceDate)
            .ThenBy(a => a.Member.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountByTeamAsync(long churchId, long teamId,
        DateOnly? from, DateOnly? to, string? search, CancellationToken ct) =>
        dbContext.TeamAttendances.AsNoTracking()
            .CountAsync(a => a.TeamId == teamId && a.Team.ChurchId == churchId
                && (from == null || a.AttendanceDate >= from)
                && (to == null || a.AttendanceDate <= to)
                && (search == null || EF.Functions.ILike(a.Member.FirstName, $"%{search}%")), ct);

    public Task<List<TeamAttendance>> GetSummaryRowsAsync(long churchId, long teamId,
        DateOnly? from, DateOnly? to, CancellationToken ct) =>
        dbContext.TeamAttendances.AsNoTracking()
            .Include(a => a.Member)
            .Where(a => a.TeamId == teamId && a.Team.ChurchId == churchId
                && (from == null || a.AttendanceDate >= from)
                && (to == null || a.AttendanceDate <= to))
            .ToListAsync(ct);

    private void ApplyChanges(List<TeamAttendance> existing, long teamId, DateOnly date,
        IReadOnlyList<(long MemberId, TeamAttendanceStatus Status, string? Reason)> entries)
    {
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
                    MemberId = memberId,
                    AttendanceDate = date,
                    Status = status,
                    Reason = reason
                });
            }
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        for (var e = ex.InnerException; e is not null; e = e.InnerException)
        {
            if (e is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                return true;
        }

        return false;
    }
}