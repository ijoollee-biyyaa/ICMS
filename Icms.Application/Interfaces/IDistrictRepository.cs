using Icms.Domain.Entities;

namespace Icms.Application.Interfaces;

public interface IDistrictRepository
{
    Task<District?> GetByIdAsync(long id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, long? excludeId, CancellationToken ct);
    Task<bool> HasChurchesAsync(long id, CancellationToken ct);
    Task<int> CountChurchesAsync(long id, CancellationToken ct);
    Task<int> CountMembersAsync(long id, CancellationToken ct);
    Task<District> AddAsync(District district, CancellationToken ct);
    Task<District> UpdateAsync(District district, CancellationToken ct);
    Task<List<District>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountAsync(string? search, CancellationToken ct);
}