using Microsoft.EntityFrameworkCore;
using Npgsql;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence;

public class TeamPaymentRepository(IcmsDbContext dbContext) : ITeamPaymentRepository
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

    public Task<bool> ExistsAsync(long teamId, long memberId, DateOnly month, CancellationToken ct) =>
        dbContext.TeamPayments.AsNoTracking()
            .AnyAsync(p => p.TeamId == teamId && p.MemberId == memberId && p.Month == month, ct);

    public Task<TeamPayment?> GetByIdAsync(long churchId, long teamId, long paymentId, CancellationToken ct) =>
        dbContext.TeamPayments
            .SingleOrDefaultAsync(p => p.Id == paymentId && p.TeamId == teamId
                && p.Team.ChurchId == churchId, ct);

    public async Task<TeamPayment> AddAsync(TeamPayment payment, CancellationToken ct)
    {
        dbContext.TeamPayments.Add(payment);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new UniqueConstraintViolationException(
                $"A payment for member {payment.MemberId} in {payment.Month:yyyy-MM} already exists.", ex);
        }

        await dbContext.Entry(payment).Reference(p => p.Member).LoadAsync(ct);
        return payment;
    }

    public async Task<TeamPayment> UpdateAsync(TeamPayment payment, CancellationToken ct)
    {
        dbContext.TeamPayments.Update(payment);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(payment).Reference(p => p.Member).LoadAsync(ct);
        return payment;
    }

    public Task<List<TeamPayment>> GetByTeamAsync(long churchId, long teamId,
        DateOnly? fromMonth, DateOnly? toMonth, string? search, int page, int pageSize, CancellationToken ct) =>
        dbContext.TeamPayments.AsNoTracking()
            .Include(p => p.Member)
            .Where(p => p.TeamId == teamId && p.Team.ChurchId == churchId
                && (fromMonth == null || p.Month >= fromMonth)
                && (toMonth == null || p.Month <= toMonth)
                && (search == null
                    || EF.Functions.ILike(p.Member.FirstName, $"%{search}%")
                    || EF.Functions.ILike(p.Member.EfgbcId, $"%{search}%")))
            .OrderByDescending(p => p.Month)
            .ThenBy(p => p.Member.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountByTeamAsync(long churchId, long teamId,
        DateOnly? fromMonth, DateOnly? toMonth, string? search, CancellationToken ct) =>
        dbContext.TeamPayments.AsNoTracking()
            .CountAsync(p => p.TeamId == teamId && p.Team.ChurchId == churchId
                && (fromMonth == null || p.Month >= fromMonth)
                && (toMonth == null || p.Month <= toMonth)
                && (search == null
                    || EF.Functions.ILike(p.Member.FirstName, $"%{search}%")
                    || EF.Functions.ILike(p.Member.EfgbcId, $"%{search}%")), ct);

    public Task<List<TeamPayment>> GetSummaryRowsAsync(long churchId, long teamId, CancellationToken ct) =>
        dbContext.TeamPayments.AsNoTracking()
            .Include(p => p.Member)
            .Where(p => p.TeamId == teamId && p.Team.ChurchId == churchId)
            .ToListAsync(ct);

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