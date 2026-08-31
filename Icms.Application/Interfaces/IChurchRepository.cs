using Icms.Domain.Entities;

namespace Icms.Application.Interfaces;

public interface IChurchRepository
{
    Task<bool> DistrictExistsAsync(long districtId, CancellationToken ct);
    Task<Church?> GetByIdAsync(long id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<Church> AddAsync(Church church, CancellationToken ct);
    Task<Church> UpdateAsync(Church church, CancellationToken ct);
    Task<bool> HasMembersAsync(long churchId, CancellationToken ct);
    Task<bool> HasDaughterChurchesAsync(long churchId, CancellationToken ct);
    Task<int> CountMembersAsync(long churchId, CancellationToken ct);
    Task<int> CountDaughterChurchesAsync(long churchId, CancellationToken ct);
    Task<IReadOnlyList<Church>> GetByDistrictPagedAsync(long districtId, string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountByDistrictAsync(long districtId, string? search, CancellationToken ct);
}