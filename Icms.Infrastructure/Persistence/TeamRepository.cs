using Microsoft.EntityFrameworkCore;
using Npgsql;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Persistence;

public class TeamRepository(IcmsDbContext dbContext) : ITeamRepository
{
    public Task<Team?> GetByIdAsync(long churchId, long id, CancellationToken ct) =>
        dbContext.Teams.SingleOrDefaultAsync(t => t.ChurchId == churchId && t.Id == id, ct);

    public Task<Church?> GetChurchAsync(long churchId, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == churchId, ct);

    public Task<bool> NameExistsAsync(long churchId, string name, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .AnyAsync(t => t.ChurchId == churchId
                && t.Name.ToLower() == name.ToLower(), ct);

    public async Task<Team> AddAsync(Team team, CancellationToken ct)
    {
        dbContext.Teams.Add(team);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw TranslateUniqueViolation(ex, team);
        }

        return team;
    }

    public async Task<Team> UpdateAsync(Team team, CancellationToken ct)
    {
        dbContext.Teams.Update(team);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw TranslateUniqueViolation(ex, team);
        }

        return team;
    }

    public Task<bool> HasMembersAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.TeamMembers.AsNoTracking()
            .AnyAsync(tm => tm.TeamId == teamId && tm.Team.ChurchId == churchId && !tm.IsDeleted && !tm.Member.IsDeleted, ct);

    public Task<bool> HasSubTeamsAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .AnyAsync(t => t.ParentTeamId == teamId && t.ChurchId == churchId, ct);

    public Task<int> CountMembersAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.TeamMembers.AsNoTracking()
            .CountAsync(tm => tm.TeamId == teamId && tm.Team.ChurchId == churchId && !tm.IsDeleted && !tm.Member.IsDeleted, ct);

    public Task<int> CountSubTeamsAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .CountAsync(t => t.ParentTeamId == teamId && t.ChurchId == churchId, ct);

    public Task<List<Team>> GetByChurchPagedAsync(
        long churchId, string? search, int page, int pageSize, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .Where(t => t.ChurchId == churchId
                && (search == null || EF.Functions.ILike(t.Name, $"%{search}%")))
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountByChurchAsync(long churchId, string? search, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .CountAsync(t => t.ChurchId == churchId
                && (search == null || EF.Functions.ILike(t.Name, $"%{search}%")), ct);

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        FindPostgresException(ex) is not null;

    private static UniqueConstraintViolationException TranslateUniqueViolation(
        DbUpdateException ex, Team team) =>
        new($"Team named '{team.Name}' already exists in this scope.", ex, FindPostgresException(ex)!.ConstraintName);

    private static PostgresException? FindPostgresException(DbUpdateException ex)
    {
        for (var e = ex.InnerException; e is not null; e = e.InnerException)
        {
            if (e is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
                return pg;
        }

        return null;
    }
}