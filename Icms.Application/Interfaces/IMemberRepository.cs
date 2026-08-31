using Icms.Domain.Entities;

namespace Icms.Application.Interfaces;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(long id, CancellationToken ct);
    Task<Church?> GetChurchAsync(long churchId, CancellationToken ct);
    Task<bool> EfgbcIdExistsAsync(string efgbcId, CancellationToken ct);
    Task<Member> AddAsync(Member member, CancellationToken ct);
    Task<Member> UpdateAsync(Member member, CancellationToken ct);
    Task<List<Member>> GetPagedAsync(
        long? churchId, string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountAsync(long? churchId, string? search, CancellationToken ct);
}