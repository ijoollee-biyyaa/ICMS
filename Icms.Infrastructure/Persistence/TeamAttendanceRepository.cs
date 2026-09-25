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
}