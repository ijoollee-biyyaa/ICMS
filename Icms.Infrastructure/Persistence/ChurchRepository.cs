using Microsoft.EntityFrameworkCore;
using Npgsql;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence;

public class ChurchRepository(IcmsDbContext dbContext) : IChurchRepository
{
    public Task<bool> DistrictExistsAsync(long districtId, CancellationToken ct) =>
        dbContext.Districts.AsNoTracking()
            .AnyAsync(d => d.Id == districtId && !d.IsDeleted, ct);

    public Task<Church?> GetByIdAsync(long id, CancellationToken ct) =>
        dbContext.Churches.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    public async Task<Church> AddAsync(Church church, CancellationToken ct)
    {
        dbContext.Churches.Add(church);

        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new UniqueConstraintViolationException(
                $"Church code '{church.Code}' already exists.", ex);
        }

        return church;
    }

    public async Task<Church> UpdateAsync(Church church, CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
        return church;
    }

    public async Task<IReadOnlyList<Church>> GetByDistrictPagedAsync(
        long districtId, string? search, int page, int pageSize, CancellationToken ct) =>
        await dbContext.Churches
            .AsNoTracking()
            .Where(c => c.DistrictId == districtId
                && (search == null
                    || EF.Functions.ILike(c.Name, $"%{search}%")
                    || EF.Functions.ILike(c.Code, $"%{search}%")))
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountByDistrictAsync(long districtId, string? search, CancellationToken ct) =>
        dbContext.Churches
            .AsNoTracking()
            .CountAsync(c => c.DistrictId == districtId
                && (search == null
                    || EF.Functions.ILike(c.Name, $"%{search}%")
                    || EF.Functions.ILike(c.Code, $"%{search}%")), ct);

    public Task<bool> HasMembersAsync(long churchId, CancellationToken ct) =>
        dbContext.Members.AsNoTracking().AnyAsync(m => m.ChurchId == churchId, ct);

    public Task<bool> HasDaughterChurchesAsync(long churchId, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking().AnyAsync(c => c.ParentChurchId == churchId, ct);

    public Task<int> CountMembersAsync(long churchId, CancellationToken ct) =>
        dbContext.Members.AsNoTracking().CountAsync(m => m.ChurchId == churchId, ct);

    public Task<int> CountDaughterChurchesAsync(long churchId, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking().CountAsync(c => c.ParentChurchId == churchId, ct);

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