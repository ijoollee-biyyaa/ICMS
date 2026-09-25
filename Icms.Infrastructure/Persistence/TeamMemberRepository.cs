using Microsoft.EntityFrameworkCore;
using Npgsql;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Persistence;

public class TeamMemberRepository(IcmsDbContext dbContext) : ITeamMemberRepository
{
    public Task<Team?> GetTeamAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .SingleOrDefaultAsync(t => t.ChurchId == churchId && t.Id == teamId, ct);

    public Task<Member?> GetMemberAsync(long memberId, CancellationToken ct) =>
        dbContext.Members.AsNoTracking()
            .SingleOrDefaultAsync(m => m.Id == memberId, ct);

    public Task<TeamMember?> GetMembershipAsync(long churchId, long teamId, long memberId, CancellationToken ct) =>
        dbContext.TeamMembers
            .SingleOrDefaultAsync(tm => tm.TeamId == teamId && tm.MemberId == memberId
                && tm.Team.ChurchId == churchId, ct);

    public Task<bool> HasSubTeamsAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.Teams.AsNoTracking()
            .AnyAsync(t => t.ParentTeamId == teamId && t.ChurchId == churchId, ct);

    public Task<int> CountMembershipsInFamilyAsync(long memberId, long rootTeamId, CancellationToken ct) =>
        dbContext.TeamMembers.AsNoTracking()
            .CountAsync(tm => tm.MemberId == memberId
                && (tm.TeamId == rootTeamId || tm.Team.ParentTeamId == rootTeamId), ct);

    public async Task<TeamMember> AddAsync(TeamMember membership, CancellationToken ct)
    {
        dbContext.TeamMembers.Add(membership);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new UniqueConstraintViolationException(
                $"Member {membership.MemberId} is already a member of team {membership.TeamId}.", ex);
        }

        return membership;
    }

    public async Task<TeamMember> UpdateAsync(TeamMember membership, CancellationToken ct)
    {
        dbContext.TeamMembers.Update(membership);
        await dbContext.SaveChangesAsync(ct);
        return membership;
    }

    public Task<List<TeamMember>> GetByTeamPagedAsync(
        long churchId, long teamId, string? search, int page, int pageSize, CancellationToken ct) =>
        dbContext.TeamMembers.AsNoTracking()
            .Include(tm => tm.Member)
            .Where(tm => tm.TeamId == teamId && tm.Team.ChurchId == churchId
                && !tm.IsDeleted
                && (search == null
                    || EF.Functions.ILike(tm.Member.FirstName, $"%{search}%")
                    || EF.Functions.ILike(tm.Member.EfgbcId, $"%{search}%")))
            .OrderBy(tm => tm.Member.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountByTeamAsync(long churchId, long teamId, string? search, CancellationToken ct) =>
        dbContext.TeamMembers.AsNoTracking()
            .CountAsync(tm => tm.TeamId == teamId && tm.Team.ChurchId == churchId
                && !tm.IsDeleted
                && (search == null
                    || EF.Functions.ILike(tm.Member.FirstName, $"%{search}%")
                    || EF.Functions.ILike(tm.Member.EfgbcId, $"%{search}%")), ct);

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