using Microsoft.EntityFrameworkCore;
using Npgsql;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence;

public class MemberRepository(IcmsDbContext dbContext) : IMemberRepository
{
    public Task<Member?> GetByIdAsync(long id, CancellationToken ct) =>
        dbContext.Members.SingleOrDefaultAsync(m => m.Id == id, ct);

    public Task<Church?> GetChurchAsync(long churchId, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking()
            .Include(c => c.District)
            .SingleOrDefaultAsync(c => c.Id == churchId, ct);

    public Task<bool> EfgbcIdExistsAsync(string efgbcId, CancellationToken ct) =>
        dbContext.Members.AsNoTracking().AnyAsync(m => m.EfgbcId == efgbcId, ct);

    public async Task<Member> AddAsync(Member member, CancellationToken ct)
    {
        dbContext.Members.Add(member);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new UniqueConstraintViolationException(
                $"Member with EFGBC ID '{member.EfgbcId}' already exists.", ex);
        }

        return member;
    }

    public async Task<Member> UpdateAsync(Member member, CancellationToken ct)
    {
        dbContext.Members.Update(member);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new UniqueConstraintViolationException(
                $"Member with EFGBC ID '{member.EfgbcId}' already exists.", ex);
        }

        return member;
    }

    public Task<List<Member>> GetPagedAsync(
        long? churchId, string? search, int page, int pageSize, CancellationToken ct) =>
        dbContext.Members.AsNoTracking()
            .Where(m => (churchId == null || m.ChurchId == churchId)
                && (search == null
                    || EF.Functions.ILike(m.EfgbcId, $"%{search}%")
                    || EF.Functions.ILike(m.FirstName, $"%{search}%")
                    || EF.Functions.ILike(m.FatherName, $"%{search}%")
                    || EF.Functions.ILike(m.GrandfatherName, $"%{search}%")))
            .OrderBy(m => m.EfgbcId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountAsync(long? churchId, string? search, CancellationToken ct) =>
        dbContext.Members.AsNoTracking()
            .CountAsync(m => (churchId == null || m.ChurchId == churchId)
                && (search == null
                    || EF.Functions.ILike(m.EfgbcId, $"%{search}%")
                    || EF.Functions.ILike(m.FirstName, $"%{search}%")
                    || EF.Functions.ILike(m.FatherName, $"%{search}%")
                    || EF.Functions.ILike(m.GrandfatherName, $"%{search}%")), ct);

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