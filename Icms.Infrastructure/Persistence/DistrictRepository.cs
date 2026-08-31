using Microsoft.EntityFrameworkCore;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence;

public class DistrictRepository(IcmsDbContext dbContext) : IDistrictRepository
{
    public Task<District?> GetByIdAsync(long id, CancellationToken ct) =>
        dbContext.Districts.AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);

    public Task<bool> CodeExistsAsync(string code, long? excludeId, CancellationToken ct) =>
        dbContext.Districts.AsNoTracking()
            .AnyAsync(d => !d.IsDeleted
                && d.Code.ToLower() == code.ToLower()
                && (excludeId == null || d.Id != excludeId), ct);

    public Task<bool> HasChurchesAsync(long id, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking()
            .AnyAsync(c => c.DistrictId == id && !c.IsDeleted, ct);

    public Task<int> CountChurchesAsync(long id, CancellationToken ct) =>
        dbContext.Churches.AsNoTracking()
            .CountAsync(c => c.DistrictId == id && !c.IsDeleted, ct);

    public Task<int> CountMembersAsync(long id, CancellationToken ct) =>
        dbContext.Members.AsNoTracking()
            .CountAsync(m => m.Church.DistrictId == id && !m.Church.IsDeleted, ct);

    public async Task<District> AddAsync(District district, CancellationToken ct)
    {
        dbContext.Districts.Add(district);
        await dbContext.SaveChangesAsync(ct);
        return district;
    }

    public async Task<District> UpdateAsync(District district, CancellationToken ct)
    {
        dbContext.Districts.Update(district);
        await dbContext.SaveChangesAsync(ct);
        return district;
    }

    public Task<List<District>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct) =>
        dbContext.Districts.AsNoTracking()
            .Where(d => !d.IsDeleted
                && (search == null
                    || EF.Functions.ILike(d.Name, $"%{search}%")
                    || EF.Functions.ILike(d.Code, $"%{search}%")))
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountAsync(string? search, CancellationToken ct) =>
        dbContext.Districts.AsNoTracking()
            .CountAsync(d => !d.IsDeleted
                && (search == null
                    || EF.Functions.ILike(d.Name, $"%{search}%")
                    || EF.Functions.ILike(d.Code, $"%{search}%")), ct);
}