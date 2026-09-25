using Microsoft.EntityFrameworkCore;
using Npgsql;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

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

    public Task<int> CountActiveMembersAsync(long churchId, CancellationToken ct) =>
        dbContext.Members.AsNoTracking().CountAsync(m => m.ChurchId == churchId && m.Status == MemberStatus.Active, ct);

    public Task<int> CountEmployeesAsync(long churchId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking().CountAsync(e => e.ChurchId == churchId && e.Status == EmployeeStatus.Active, ct);

    public Task<int> CountMinistersAsync(long churchId, CancellationToken ct) =>
        dbContext.Employees.AsNoTracking().CountAsync(e => e.ChurchId == churchId && e.EmploymentType == EmploymentType.FulltimeMinister && e.Status == EmployeeStatus.Active, ct);

    public Task<int> CountDaughterChurchesAsync(long churchId, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking().CountAsync(c => c.ParentChurchId == churchId, ct);

    public async Task<Dictionary<long, (int MemberCount, int EmployeeCount, int MinisterCount)>> GetChurchMetricsAsync(IEnumerable<long> churchIds, CancellationToken ct)
    {
        var idList = churchIds.Distinct().ToList();
        if (idList.Count == 0) return [];

        var memberCounts = await dbContext.Members.AsNoTracking()
            .Where(m => idList.Contains(m.ChurchId) && m.Status == MemberStatus.Active)
            .GroupBy(m => m.ChurchId)
            .Select(g => new { ChurchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ChurchId, x => x.Count, ct);

        var employeeCounts = await dbContext.Employees.AsNoTracking()
            .Where(e => e.ChurchId.HasValue && idList.Contains(e.ChurchId.Value) && e.Status == EmployeeStatus.Active)
            .GroupBy(e => e.ChurchId!.Value)
            .Select(g => new { ChurchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ChurchId, x => x.Count, ct);

        var ministerCounts = await dbContext.Employees.AsNoTracking()
            .Where(e => e.ChurchId.HasValue && idList.Contains(e.ChurchId.Value) && e.EmploymentType == EmploymentType.FulltimeMinister && e.Status == EmployeeStatus.Active)
            .GroupBy(e => e.ChurchId!.Value)
            .Select(g => new { ChurchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ChurchId, x => x.Count, ct);

        var result = new Dictionary<long, (int MemberCount, int EmployeeCount, int MinisterCount)>();
        foreach (var id in idList)
        {
            memberCounts.TryGetValue(id, out var mc);
            employeeCounts.TryGetValue(id, out var ec);
            ministerCounts.TryGetValue(id, out var minC);
            result[id] = (mc, ec, minC);
        }

        return result;
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